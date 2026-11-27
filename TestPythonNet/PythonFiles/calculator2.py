def add(a, b):
    return a + b

def multiply(a, b):
    return a * b

def divide(a, b):
    return a / b

def greet(name):
    return f"Hello {name}!"

def calculate(price, quantity=1, discount=0):
    return price * quantity * (1 - discount)

def get_measurement():
    return 12.5, 24.8, True

def get_values():
    return [10, 20, 30, 40, 50]

def get_configuration():
    return {
        "hostname": "localhost",
        "port": 8080,
        "enabled": True
    }

def process_values(values):
    return {
        "min": min(values),
        "max": max(values),
        "sum": sum(values),
        "average": sum(values) / len(values)
    }

def execute_task(callback):
    for i in range(5):
        callback(i)

    return "Completed"

def find_device():
    return None