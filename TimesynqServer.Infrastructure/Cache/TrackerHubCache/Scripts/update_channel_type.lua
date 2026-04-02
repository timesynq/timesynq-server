-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, Channel, IsSend, UpdatedOnUTC

local default_line_count = 64
local default_lines_per_beat = 4
local default_send_mask = "0000"

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_index_key = "tracker:room:" .. wip_id .. ":index"

local frame_key = "tracker:room:" .. wip_id .. ":frame:" .. input.Frame
local frame_exists = redis.call("EXISTS", frame_key)
if frame_exists == 0 then
	redis.call("HSET", frame_key,
		"LineCount", default_line_count,
		"LinesPerBeat", default_lines_per_beat,
		"SendMask", default_send_mask
	)
	redis.call("SADD", room_index_key, frame_key)
end

local old_send_mask = redis.call("HGET", frame_key, "SendMask")
local old_send_mask_short = tonumber(old_send_mask, 16)

local index_number = tonumber(input.Channel)
if not index_number then
	return nil
end
index_number -= 1

local bit = input.IsSend and input.bit.lshift(1, index_number) or 0
local mask = bit.lshift(1, index_number)
local cleared_send_mask_short = bit.band(old_send_mask_short, bit.bnot(mask))
local new_send_mask_short = bit.bor(bit, cleared_send_mask_short)

local new_send_mask = string.format("%x", new_send_mask_short)

redis.call("HSET", frame_key,
	"SendMask", new_send_mask
)

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	Type = "send_mask"
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_send_mask,	
	NewValue = new_send_mask,
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id