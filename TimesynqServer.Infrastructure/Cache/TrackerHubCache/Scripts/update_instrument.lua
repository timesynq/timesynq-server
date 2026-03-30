-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, Channel, Line, NoteGroup, Address, NewInstrument, UpdatedOnUTC

local empty_line_value = string.rep("-", 28)

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local channel_key = "tracker:room:" .. wip_id .. ":frame:" .. input.Frame .. ":channel:" .. input.Channel
local old_line_value = redis.call("HGET", channel_key, input.Line)
old_line_value = old_line_value and old_line_value or empty_line_value

local note_group = tonumber(input.NoteGroup)

local left = string.sub(old_line_value, 1, (note_group * 4) + 3)
local old_instrument_value = string.sub(old_line_value, (note_group * 4) + 3, (note_group * 4) + 5)
local right = string.sub(old_line_value, (note_group * 4) + 5)
local new_line_value = left .. input.NewPitch .. right

if new_line_value == empty_line_value then
	redis.call("HDEL", channel_key, input.Line)
else
	redis.call("HSET", channel_key,
		input.Line, new_line_value
	)
end

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_instrument_value,	
	NewValue = input.NewPitch,
	Address = input.Address
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id