local cw = 640
local ch = 640
local gw = 64
local gh = 64
local w = 10
local h = 10
local s = w * h
local qp = {}
local qz = 1

--浮点数转整数
local function f_i(f)
	return f - (f % 1);
end

function init()
	for i = 1, s do
		qp[i] = 0
	end
	qz = 1
end

function run()
	for i = 1, s do
		local x = f_i((i - 1) % w) * gw
		local y = f_i((i - 1) / w) * gh
		if qp[i] == 0 then
			c_draw_rectangle(x, y, x + gw, y + gh, 255, 255, 255)
		elseif qp[i] == 1 then
			c_draw_ellipse(x, y, x + gw, y + gh, 0, 0, 0)
		elseif qp[i] == 2 then
			c_draw_ellipse(x, y, x + gw, y + gh, 255, 255, 255)
		end
	end

	local down, x, y = c_input()
	if down == 1 then
		local lx = f_i(x / gw)
		local ly = f_i(y / gh)
		if qp[lx + ly * w + 1] ~= 0 then
			c_message("不能在有子的地方落子");
		else
			qp[lx + ly * w + 1] = qz
			if qz == 1 then
				qz = 2
			else
				qz = 1
			end
		end
	end
end
