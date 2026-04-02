-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, NewChannelCount, UpdatedOnUTC

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local room_info_key = "tracker:room:" .. wip_id .. ":info"
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

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	Type = "channel_count",
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_channel_count_number,	
	NewValue = new_channel_count_number,
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id