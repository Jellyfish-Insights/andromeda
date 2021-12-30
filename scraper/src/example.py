class X:
	def __init__(self):
		self.a = 42

class Y(X):
	def __init__(self, other):
		self.__dict__ = other.__dict__

	def y_method(self):
		print("this is an instance of Y")
		print(f"a is {self.a}")

x = X()
y = Y(x)
print(y)
print(type(y))
y.y_method()