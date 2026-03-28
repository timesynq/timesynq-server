-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- script layout:
-- use connectionKey to get the roomCode, if that's not found it will immediately return nothing
-- locate the cell that is being updated.  if the channel or line does not exist, create it
-- if the line after updating is just 28 dashes, remove it from the key. the entire channel should be automatically deleted if theres no entries.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, Channel, Line, NoteGroup, NewPitch

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local channel_key = "tracker:room:" .. wip_id .. ":frame:" .. input.Frame .. ":channel:" .. frame.Channel
local old_line_value = redis.call("HGET", channel_key, input.Line)
old_line_value = old_line_value and old_line_value or "----------------------------"

local note_group = tonumber(input.NoteGroup)

local left = note_group ~= 0 and string.sub(old_line_value, 1, (note_group * 4) + 1) or ""
local right = string.sub(old_line_value, (note_group * 4) + 3)
local new_line_value = left .. input.NewPitch .. right

redis.call("HSET", channel_key,
	input.Line, new_line_value
)

-- create operational log entry and save it
local room_log_key = "tracker:room:" .. wip_id .. ":log"


return wip_id