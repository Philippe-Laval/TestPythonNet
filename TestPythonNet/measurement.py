class MeasurementResult:

    def __init__(self, values):
        self.values = values
        self.minimum = min(values)
        self.maximum = max(values)
        self.average = sum(values) / len(values)

    def is_valid(self, threshold):
        return self.maximum <= threshold

    def __str__(self):
        return f"Min={self.minimum}, Max={self.maximum}, Average={self.average}"


def analyze(values):
    result = MeasurementResult(values)
    return result