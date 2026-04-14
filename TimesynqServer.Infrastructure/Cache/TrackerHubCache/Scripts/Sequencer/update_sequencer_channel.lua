-- IMPORTANT: This script is incompatible with Redis Cluster as we are accessing data across different hash slots.

-- KEYS[1] = connectionKey

-- ARGV[1] = JSON serialized payload that contains
-- UserId, Line, Channel, IsOn, UpdatedOnUTC

-- LIB IMPORTS
-- connection.lua: connection_field_names{}
-- room.lua: get_room_sequencer_key()
-- sequencer.lua: sequencer_defaults{}
-- operation_log.lua: add_operation_log_entry()

local input = cjson.decode(ARGV[1])

local wip_id = redis.call("HGET", KEYS[1], connection_field_names.wip_id)
if not wip_id then
	return nil
end

local sequencer_key = get_room_sequencer_key(wip_id)
local old_sequencer_line_value = redis.call("HGET", sequencer_key, input.Line)
old_sequencer_line_value = old_sequencer_line_value and old_sequencer_line_value or sequencer_defaults.line_value

local left = string.sub(old_sequencer_line_value, 1, 2)
local old_mask = string.sub(old_sequencer_line_value, 3)
local old_mask_short = tonumber(old_mask, 16)

local index_number = tonumber(input.Channel)
if not index_number then
	return nil
end
index_number = index_number - 1

local is_on = string.lower(input.IsOn) == "true"

local new_bit = is_on and bit.lshift(1, index_number) or 0
local mask = bit.lshift(1, index_number)
local cleared_mask_short = bit.band(old_mask_short, bit.bnot(mask))
local new_mask_short = bit.bor(new_bit, cleared_mask_short)

local new_mask = string.upper(string.format("%04x", new_mask_short))

local new_sequencer_line_value = left .. new_mask

redis.call("HSET", sequencer_key, 
	input.Line, new_sequencer_line_value
)

local debug = {
	OldMask = old_mask,
	OldMaskShort = old_mask_short,
	IndexNumber = index_number,
	InputIsOn = string.lower(input.IsOn),
	NewBit = new_bit,
	Mask = mask,
	CMS = cleared_mask_short,
	NMS = new_mask_short,
	NewMask = new_mask
}
redis.call("RPUSH", "DEBUG",
	cjson.encode(debug)
)

add_operation_log_entry(
	wip_id,
	"sequencer_channel",
	input.UserId,
	input.UpdatedOnUTC,
	old_mask,
	new_mask,
	input.Line
)

return wip_id