-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Type, Frame, Channel, Line, Column, Address, NewValue, UpdatedOnUTC

local num_columns = 14
local num_chars_per_column = 2
local default_line_count = 64
local default_lines_per_beat = 4

local input = cjson.decode(ARGV[1])

num_columns = input.Channel == "00" and 8 or num_columns

local empty_line_value = string.rep("-", num_columns * num_chars_per_column)

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_index_key = "tracker:room:" .. wip_id .. ":index"

local column = tonumber(input.Column)
if not column or column < 0 or column >= num_columns then 
	return nil
end

local frame_key = "tracker:room:" .. wip_id .. ":frame:" .. input.Frame
local frame_exists = redis.call("EXISTS", frame_key)
if frame_exists == 0 then
	redis.call("HSET", frame_key,
		"LineCount", default_line_count,
		"LinesPerBeat", default_lines_per_beat
	)
	redis.call("SADD", room_index_key, frame_key)
end

local channel_key = frame_key .. ":channel:" .. input.Channel
local channel_exists = redis.call("EXISTS", channel_key)
if channel_exists == 0 then
	redis.call("SADD", room_index_key, channel_key)
end

local old_line_value = redis.call("HGET", channel_key, input.Line)
old_line_value = old_line_value and old_line_value or empty_line_value

local left_start = 1
local left_end = (column * num_chars_per_column) + 1
local right_start = (column * num_chars_per_column) + num_chars_per_column + 1

local left = column ~= 0 and string.sub(old_line_value, left_start, left_end) or ""
local old_pitch_value = string.sub(old_line_value, left_end, right_start)
local right = column ~= num_columns - 1 and string.sub(old_line_value, right_start) or ""
local new_line_value = left .. input.NewValue .. right

if new_line_value == empty_line_value then
	redis.call("HDEL", channel_key, input.Line)
else
	redis.call("HSET", channel_key,
		input.Line, new_line_value
	)
end

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	Type = input.Type,
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_value,	
	NewValue = input.NewValue,
	Address = input.Address
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id