local function build_key(...)
	local segments = {...}
	return table.concat(segments, ":")
end