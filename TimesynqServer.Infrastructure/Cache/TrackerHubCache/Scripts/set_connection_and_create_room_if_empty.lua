-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey
-- KEYS[2] = roomIndexKey
-- KEYS[3] = roomConnectionsKey
-- KEYS[4] = roomInfoKey

-- ARGV[1] = JSON serialized payload that contains
-- WipId, ConnectionId, UserId, UserName, WipName, OwnerId, WipBpm

local input = cjson.decode(ARGV[1])

redis.call("HSET", KEYS[1],
	"WipId", input.WipId,
	"ConnectionId", input.ConnectionId,
	"UserId", input.UserId,
	"UserName", input.UserName
)

local room_exists = redis.call("EXISTS", KEYS[2])

if room_exists == 0 then
	redis.call("HSET", KEYS[4],
		"WipName", input.WipName,
		"OwnerId", input.OwnerId
		"Bpm", input.WipBpm
	)
	redis.call("SADD", KEYS[2], KEYS[4])
	-- create first frame info
	-- add first frame info to room index
end

local room_keys = redis.call("SMEMBERS", KEYS[2])
redis.call("PERSIST", KEYS[2])
for _, room_key in ipairs(room_keys) do
	redis.call("PERSIST", room_key)
end

local existing_connections = redis.call("SMEMBERS", KEYS[3])
local users = setmetatable({}, cjson.array_mt)

for _, connection_key in ipairs(existing_connections) do
    local user_data = redis.call("HGETALL", connection_key)
    
    if next(user_data) ~= nil then
        local user = {}
        for i = 1, #user_data, 2 do
            user[user_data[i]] = user_data[i + 1]
        end
        
        table.insert(users, user)
    end
end

redis.call("SADD", KEYS[3], KEYS[1])

if #users == 0 then
	return "[]"
end
return cjson.encode(users)
