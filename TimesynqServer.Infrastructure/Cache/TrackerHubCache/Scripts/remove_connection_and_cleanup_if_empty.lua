-- KEYS[1] = connectionKey
-- KEYS[2] = roomKey
-- KEYS[3] = roomConnectionsSetKey

-- ARGV[1] = userId
-- ARGV[2] = ttlSeconds

redis.call("DEL", KEYS[1])
redis.call("SREM", KEYS[3], ARGV[1])

local remainingUsers = redis.call("SCARD", KEYS[3])

if remainingUsers == 0 then
	redis.call("EXPIRE", KEYS[2], tonumber(ARGV[2]))
	return 1
end

return 0