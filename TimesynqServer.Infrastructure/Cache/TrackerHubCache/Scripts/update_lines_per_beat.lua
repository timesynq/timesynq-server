-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, NewLinesPerBeat, UpdatedOnUTC

-- LIB IMPORTS
-- connection.lua: connection_field_names{}
-- room.lua: get_room_index_key()
-- frame.lua: lines_per_beat_key, get_frame_key_and_create_frame_if_nonexistent()
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], connection_field_names.wip_id)
if not wip_id then
	return nil
end

local room_index_key = get_room_index_key(wip_id)
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

add_operation_log_entry(
	wip_id, 
	"lines_per_beat", 
	input.UserId,
	input.UpdatedOnUTC, 
	old_lines_per_beat_number,
	new_lines_per_beat_number
)

return wip_id