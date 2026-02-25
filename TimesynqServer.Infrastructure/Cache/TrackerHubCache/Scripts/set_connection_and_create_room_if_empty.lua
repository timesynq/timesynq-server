-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey
-- KEYS[2] = roomIndexKey
-- KEYS[3] = roomConnectionsKey
-- KEYS[4] = roomInfoKey

-- ARGV[1] = wipId
-- ARGV[2] = connectionId

-- ARGV[3] = wipName field name
-- ARGV[4] = wipName
-- ARGV[5] = ownerId field name
-- ARGV[6] = ownerId

redis.call("SET", KEYS[1], ARGV[1])

local room_exists = redis.call("EXISTS", KEYS[2])

if room_exists == 0 then
	redis.call("HSET", KEYS[4], ARGV[3], ARGV[4], ARGV[5], ARGV[6])
	redis.call("SADD", KEYS[2], KEYS[4])
	-- create first frame info
	-- add first frame info to room index
end

redis.call("SADD", KEYS[3], KEYS[1])

local room_keys = redis.call("SMEMBERS", KEYS[2])
redis.call("PERSIST", KEYS[2])
for _, room_key in ipairs(room_keys) do
	redis.call("PERSIST", room_key)
end

return 0
