-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, NewSequencerLength, UpdatedOnUTC

-- LIB IMPORTS
-- connection.lua: connection_field_names{}
-- room.lua: get_room_info_key(), room_info_field_names{}
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], connection_field_names.wip_id)
if not wip_id then
	return nil
end

local room_info_key = get_room_info_key(wip_id)
local old_sequencer_length = redis.call("HGET", room_info_key, room_info_field_names.sequencer_length)
if not old_sequencer_length then
	return nil
end

local old_sequencer_length_number = tonumber(old_sequencer_length)
local new_sequencer_length_number = tonumber(input.NewSequencerLength)
if not new_sequencer_length_number then
	return nil
end

redis.call("HSET", room_info_key, 
	room_info_field_names.sequencer_length, new_sequencer_length_number
)

add_operation_log_entry(
	wip_id,
	"sequencer_length",
	input.UserId,
	input.UpdatedOnUTC,
	old_sequencer_length_number,
	new_sequencer_length_number
)

return wip_id