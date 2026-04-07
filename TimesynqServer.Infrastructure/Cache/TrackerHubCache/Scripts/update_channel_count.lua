-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, NewChannelCount, UpdatedOnUTC

-- LIB IMPORTS
-- room_keys.lua: get_room_info_key()
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_info_key = get_room_info_key(wip_id)
local old_channel_count = redis.call("HGET", room_info_key, "Channels")
if not old_channel_count then
	return nil
end

local old_channel_count_number = tonumber(old_channel_count)
local new_channel_count_number = tonumber(input.NewChannelCount)
if not new_channel_count_number then
	return nil
end

redis.call("HSET", room_info_key, 
	"Channels", new_channel_count_number
)

add_operation_log_entry(
	wip_id,
	"channel_count",
	input.UserId,
	input.UpdatedOnUTC,
	old_channel_count_number,
	new_channel_count_number
)

return wip_id