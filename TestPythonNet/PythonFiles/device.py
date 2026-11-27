class Device:
    def __init__(self, name):
        self.name = name
        self.connected = False

    def connect(self):
        self.connected = True

    def disconnect(self):
        self.connected = False

    def get_status(self):
        return {
            "name": self.name,
            "connected": self.connected
        }

    @property
    def status(self):
        return "Online" if self.connected else "Offline"