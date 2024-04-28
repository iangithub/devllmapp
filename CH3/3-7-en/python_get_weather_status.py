
from promptflow import tool
from configparser import ConfigParser
import requests

config = ConfigParser()
config.read('config.ini')

# The inputs section will change based on the arguments of the tool function, after you save the code
# Adding type to arguments and return value will help the system show the types properly
# Please update the function name/signature per need
@tool
def my_python_tool() -> str:
    # call openweather api to get weather in Taipei
    url = "http://api.openweathermap.org/data/2.5/weather?q=Taipei"
    params = {
        'appid': config['OpenWeatherMap']['API_KEY'],
        'units': 'metric',
        # 'lang': 'zh_tw'
    }
    response = requests.get(url, params=params, timeout=5)
    print(response.json())
    return response.json()['weather'][0]['description']
