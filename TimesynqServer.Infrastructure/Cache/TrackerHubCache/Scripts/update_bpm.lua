-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, NewBpm, UpdatedOnUTC

-- LIB IMPORTS
-- room_keys.lua: get_room_info_key()
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_info_key = get_room_info_key(wip_id)
local old_bpm_value = redis.call("HGET", room_info_key, "Bpm")
if not old_bpm_value then
	return nil
end

local old_bpm_number = tonumber(old_bpm_value)
local new_bpm_number = tonumber(input.NewBpm)
if not new_bpm_number then
	return nil
end

redis.call("HSET", room_info_key, 
	"Bpm", new_bpm_number
)

add_operation_log_entry(
	wip_id,
	"bpm",
	input.UserId,
	input.UpdatedOnUTC,
	old_bpm_number,
	new_bpm_number
)

return wip_id