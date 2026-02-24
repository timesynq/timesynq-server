-- IMPORTANT: Because of how room info is stored, we cannot provide deterministic room keys to this script, instead opting to build them here.
-- This means that this script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = ttlSeconds

local wip_id = redis.call("GET", KEYS[1])
if not wip_id then
	return nil
end

local room_connections_key = "tracker:room:" .. wip_id ..":connections"

redis.call("DEL", KEYS[1])
redis.call("SREM", room_connections_key, KEYS[1])

local remaining_users = redis.call("SCARD", room_connections_key)

if remaining_users == 0 then
	local room_index_key = "tracker:room:" .. wip_id .. ":index"
	local room_keys = redis.call("SMEMBERS", room_index_key)
	for _, room_key in ipairs(room_keys) do
		redis.call("EXPIRE", room_key, tonumber(ARGV[1]))
	end
end

return wip_id