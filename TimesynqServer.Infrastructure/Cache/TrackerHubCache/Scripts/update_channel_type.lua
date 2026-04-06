-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, Channel, IsSend, UpdatedOnUTC

-- LIB IMPORTS
-- frame.lua: send_mask_key, get_frame_key_and_create_frame_if_nonexistent()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_index_key = "tracker:room:" .. wip_id .. ":index"
local frame_key =  get_frame_key_and_create_frame_if_nonexistent(wip_id, input.Frame, room_index_key)

local old_send_mask = redis.call("HGET", frame_key, send_mask_key)
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
	send_mask_key, new_send_mask
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