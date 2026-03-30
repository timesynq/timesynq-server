-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, NewLineCount, UpdatedOnUTC

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local frame_key = "tracker:room:" .. wip_id .. ":frame:" .. input.Frame
local old_line_count = redis.call("HGET", frame_key, "LineCount")
if not old_line_count then
	return nil
end

local old_line_count_number = tonumber(old_line_count)
local new_line_count_number = tonumber(input.NewLineCount)
if not new_line_count_number then
	return nil
end

redis.call("HSET", frame_key, 
	"LineCount", new_line_count_number
)

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_line_count_number,	
	NewValue = new_line_count_number,
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id