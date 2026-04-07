-- LIB IMPORTS
-- room_keys.lua: get_room_frame_key()

local default_line_count = 64
local default_lines_per_beat = 4
local default_send_mask = "0000"

local line_count_key = "LineCount"
local lines_per_beat_key = "LinesPerBeat"
local send_mask_key = "SendMask"

function get_frame_key_and_create_frame_if_nonexistent(wip_id, frame_hex, room_index_key)
	local frame_key = get_room_frame_key(wip_id, frame_hex)
	local frame_exists = redis.call("EXISTS", frame_key)
	if frame_exists == 0 then
		redis.call("HSET", frame_key,
			line_count_key, default_line_count,
			lines_per_beat_key, default_lines_per_beat,
			send_mask_key, default_send_mask
		)
		redis.call("SADD", room_index_key, frame_key)
	end
	return frame_key
end