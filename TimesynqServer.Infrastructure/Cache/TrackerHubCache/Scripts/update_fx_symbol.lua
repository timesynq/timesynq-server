-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Frame, Channel, Line, FXGroup, Address, NewFXSymbol, UpdatedOnUTC

local fx_groups_offset = 13
local empty_line_value = string.rep("-", 28)

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], "WipId")
if not wip_id then
	return nil
end

local channel_key = "tracker:room:" .. wip_id .. ":frame:" .. input.Frame .. ":channel:" .. input.Channel
local old_line_value = redis.call("HGET", channel_key, input.Line)
old_line_value = old_line_value and old_line_value or empty_line_value

local fx_group = tonumber(input.FXGroup)

local left = string.sub(old_line_value, 1, fx_groups_offset + (fx_group * 4))
local old_fx_symbol = string.sub(old_line_value, fx_groups_offset + (fx_group * 4), fx_groups_offset + (fx_group * 4) + 2)
local right = string.sub(old_line_value, fx_groups_offset + (fx_group * 4) + 2)
local new_line_value = left .. input.NewFXSymbol .. right

if new_line_value == empty_line_value then
	redis.call("HDEL", channel_key, input.Line)
else
	redis.call("HSET", channel_key,
		input.Line, new_line_value
	)
end

local room_log_key = "tracker:room:" .. wip_id .. ":log"
local operation_log_entry = {
	Type = "fx_symbol",
	UserId = input.UserId,
	Timestamp = input.UpdatedOnUTC,
	OldValue = old_fx_symbol,	
	NewValue = input.NewFXSymbol,
	Address = input.Address
}
local operation_log_entry_json = cjson.encode(operation_log_entry)
redis.call("RPUSH", room_log_key, operation_log_entry_json)

return wip_id