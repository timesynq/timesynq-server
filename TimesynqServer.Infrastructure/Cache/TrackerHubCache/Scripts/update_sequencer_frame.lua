-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Line, NewFrame, UpdatedOnUTC

-- LIB IMPORTS
-- connection.lua: connection_field_names{}
-- room.lua: get_room_sequencer_key()
-- sequencer.lua: sequencer_defaults{}
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], connection_field_names.wip_id)
if not wip_id then
	return nil
end

local sequencer_key = get_room_sequencer_key(wip_id)
local old_sequencer_line_value = redis.call("HGET", sequencer_key, input.Line)
old_sequencer_line_value = old_sequencer_line_value and old_sequencer_line_value or sequencer_defaults.line_value

local old_frame = string.sub(old_sequencer_line_value, 1, 3)
local right = string.sub(old_sequencer_line_value, 3)
local new_sequencer_line_value = input.NewFrame .. right

redis.call("HSET", sequencer_key, 
	input.Line, new_sequencer_line_value
)

add_operation_log_entry(
	wip_id,
	"sequencer_frame",
	input.UserId,
	input.UpdatedOnUTC,
	old_frame,
	new_frame,
	input.Line
)

return wip_id