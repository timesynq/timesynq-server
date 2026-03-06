-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = roomIndexKey
-- KEYS[2] = roomConnectionsKey

local room_keys = redis.call("SMEMBERS", KEYS[1])
for _, room_key in ipairs(room_keys) do
	redis.call("DEL", room_key)
end

local connection_keys = redis.call("SMEMBERS", KEYS[2])
for _, connection_key in ipairs(connection_keys) do
	redis.call("DEL", connection_key)
end

redis.call("DEL", KEYS[1])
redis.call("DEL", KEYS[2])

return 0