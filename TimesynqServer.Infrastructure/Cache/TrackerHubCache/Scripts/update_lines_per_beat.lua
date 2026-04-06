-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, NewLinesPerBeat, UpdatedOnUTC

-- LIB IMPORTS
-- frame.lua: lines_per_beat_key, get_frame_key_and_create_frame_if_nonexistent()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_index_key = "tracker:room:" .. wip_id .. ":index"
local frame_key =  get_frame_key_and_create_frame_if_nonexistent(wip_id, input.Frame, room_index_key)

local old_lines_per_beat = redis.call("HGET", frame_key, lines_per_beat_key)

local old_lines_per_beat_number = tonumber(old_lines_per_beat)
local new_lines_per_beat_number = tonumber(input.NewLinesPerBeat)
if not new_lines_per_beat_number then
	return nil
end

redis.call("HSET", frame_key,
	lines_per_beat_key, new_lines_per_beat_number
)

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	Type = "lines_per_beat"
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_lines_per_beat_number,	
	NewValue = new_lines_per_beat_number,
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id