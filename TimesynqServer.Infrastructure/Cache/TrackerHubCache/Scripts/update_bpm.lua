-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, NewBpm, UpdatedOnUTC

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_info_key = "tracker:room:" .. wip_id .. ":info"
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

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_bpm_number,	
	NewValue = new_bpm_number,
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id