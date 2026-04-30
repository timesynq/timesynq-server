-- LIB IMPORTS
-- room.lua: get_room_frame_key()

local frame_defaults = {
	line_count = 64,
	lines_per_beat = 4,
	send_mask = "0000",
	on_mask = "FFFF",
	solo_mask = "0000"
}

local frame_field_names = {
	line_count = "LineCount",
	lines_per_beat = "LinesPerBeat",
	send_mask = "SendMask",
	on_mask = "OnMask",
	solo_mask = "SoloMask"
}

local function get_frame_key_and_create_frame_if_nonexistent(wip_id, frame_hex, room_index_key)
	local frame_key = get_room_frame_key(wip_id, frame_hex)
	local frame_exists = redis.call("EXISTS", frame_key)
	if frame_exists == 0 then
		redis.call("HSET", frame_key,
			frame_field_names.line_count, frame_defaults.line_count,
			frame_field_names.lines_per_beat, frame_defaults.lines_per_beat,
			frame_field_names.send_mask, frame_defaults.send_mask,
			frame_field_names.on_mask, frame_defaults.on_mask,
			frame_field_names.solo_mask, frame_defaults.solo_mask
		)
		redis.call("SADD", room_index_key, frame_key)
	end
	return frame_key
end