def f(*args, **kwargs):
	print(args)
	print(kwargs)
	print(*args)
	print(*kwargs)
	g(*args, **kwargs)

def g(pos1, pos2, /, a = None, b = None):
	print(f"{pos1=}")
	print(f"{pos2=}")
	print(f"{a=}")
	print(f"{b=}")

f(1, 2, a = 33, b= 64)