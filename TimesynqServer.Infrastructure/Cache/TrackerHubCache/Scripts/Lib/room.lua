-- LIB IMPORTS
-- key_builder.lua: build_key()

local tracker_segment = "tracker"
local room_segment = "room"

function get_room_connections_key(wip_id)
	return build_key(tracker_segment, room_segment, wip_id, "connections")
end

function get_room_index_key(wip_id)
	return build_key(tracker_segment, room_segment, wip_id, "index")
end

function get_room_info_key(wip_id)
	return build_key(tracker_segment, room_segment, wip_id, "info")
end

local room_info_field_names = {
	wip_name = "WipName",
	owner_id = "OwnerId",
	bpm = "Bpm",
	channels = "Channels"
}

function get_room_log_key(wip_id)
	return build_key(tracker_segment, room_segment, wip_id, "log")
end

function get_room_frame_key(wip_id, frame_hex)
	return build_key(tracker_segment, room_segment, wip_id, "frame", frame_hex)
end