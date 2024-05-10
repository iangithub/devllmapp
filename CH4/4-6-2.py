from langchain_core.prompts import PromptTemplate
from langchain.tools import tool
from langchain.agents import AgentExecutor, create_react_agent
from langchain_openai import AzureChatOpenAI
from langchain.tools import BaseTool
from typing import Optional, Union



@tool
def circle_area(radius) -> float:
    """計算圓形的面積時，使用這個工具。"""
    return float(radius)**2*3.14


desc = (
    "use this tool when you need to calculate the area of a triangle"
    "To use the tool, you must provide all of the following parameters "
    "{'a_side', 'b_side', 'c_side'}."
)

class TriangleTool(BaseTool):
    name = "Triangle area calculator"
    description = desc
    
    def _run(
        self,
        sides: Optional[Union[int, float, str]] = None,
    ):
        import ast
        parsed_dict = ast.literal_eval(sides)

        a_side = parsed_dict['a_side']
        b_side = parsed_dict['b_side']
        c_side = parsed_dict['c_side']

        if not all([a_side, b_side, c_side]):
            print(a_side, b_side, c_side)
            raise ValueError("You must provide all three sides of the triangle")
        if a_side <= 0 or b_side <= 0 or c_side <= 0:
            raise ValueError("All sides must be greater than 0")
        if a_side + b_side <= c_side or a_side + c_side <= b_side or b_side + c_side <= a_side:
            raise ValueError("The sum of any two sides must be greater than the third side")
        # calculate the area
        s = (a_side + b_side + c_side) / 2
        area = (s * (s - a_side) * (s - b_side) * (s - c_side)) ** 0.5
        return area
    


tools = [circle_area, TriangleTool()]

prompt = PromptTemplate.from_template("""Answer the following questions as best you can. You have access to the following tools:

{tools}

Use the following format:

Question: the input question you must answer
Thought: you should always think about what to do
Action: the action to take, should be one of [{tool_names}]
Action Input: the input to the action
Observation: the result of the action
... (this Thought/Action/Action Input/Observation can repeat N times)
Thought: I now know the final answer
Final Answer: the final answer to the original input question

Begin!

                                      
Question: {input}
Thought:{agent_scratchpad}
""")

model = AzureChatOpenAI(
    api_key="xx",
    openai_api_version="2024-02-15-preview",
    azure_deployment="gpt-4-0125",
    azure_endpoint="https://langchainbook.openai.azure.com/",
    temperature=0,
)

zero_shot_agent = create_react_agent(
    llm=model,
    tools=tools,
    prompt=prompt,
)

agent_executor = AgentExecutor(agent=zero_shot_agent, tools=tools, verbose=True)

response = agent_executor.invoke({"input": "請問一個三邊為 3, 4, 5 的三角形，面積是多少？"})

print(response)

response = agent_executor.invoke({"input": "請問半徑為 10 圓形的面積是多少？"})
print(response)


