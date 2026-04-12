-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, NewLineCount, UpdatedOnUTC

-- LIB IMPORTS
-- connection.lua: connection_field_names{}
-- room.lua: get_room_index_key()
-- frame.lua: frame_field_names{}, get_frame_key_and_create_frame_if_nonexistent()
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], connection_field_names.wip_id)
if not wip_id then
	return nil
end

local room_index_key = get_room_index_key(wip_id)
local frame_key =  get_frame_key_and_create_frame_if_nonexistent(wip_id, input.Frame, room_index_key)

local old_line_count = redis.call("HGET", frame_key, frame_field_names.line_count)

local old_line_count_number = tonumber(old_line_count)
local new_line_count_number = tonumber(input.NewLineCount)
if not new_line_count_number then
	return nil
end

redis.call("HSET", frame_key,
	frame_field_names.line_count, new_line_count_number
)

add_operation_log_entry(
	wip_id,
	"line_count",
	input.UserId,
	input.UpdatedOnUTC,
	old_line_count_number,
	new_line_count_number
)

return wip_id