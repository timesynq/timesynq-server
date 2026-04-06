function add_operation_log_entry(wip_id, type, user_id, timestamp, old_value, new_value, address)
	local room_log_key = "tracker:room:" .. wip_id .. ":log"
	local operation_log_entry = {
		Type = type,
		UserId = user_id,
		Timestamp = timestamp,
		OldValue = old_value,
		NewValue = new_value,
	}
	if address ~= nil then
		operation_log_entry.Address = address
	end
	redis.call("RPUSH", room_log_key, cjson.encode(operation_log_entry))
end