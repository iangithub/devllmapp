from langchain_core.prompts.chat import ChatPromptTemplate
from langchain_core.prompts import PromptTemplate
from langchain.chains import LLMChain
from langchain.chains import LLMMathChain
from langchain.agents import Tool
from langchain.agents import AgentExecutor, create_react_agent
from langchain_openai import AzureChatOpenAI


chat_template = ChatPromptTemplate.from_messages([
    ("system", "你是熱心助人的 AI，同時也是地理高手。"),
    ("human", "你好！"),
    ("ai", "你好~"),
    ("human", "{user_input}"),
])


model = AzureChatOpenAI(
    api_key="yourkey",
    openai_api_version="2024-02-15-preview",
    azure_deployment="gpt-4-0125",
    azure_endpoint="https://langchainbook.openai.azure.com/",
    temperature=0,
)

llm_math = LLMMathChain.from_llm(model)
llm_chain = LLMChain(llm=model, prompt=chat_template)


math_tool = Tool(
    name="Calculator",
    func=llm_math.run,
    description="你是計算數學問題的好用工具，告訴我計算出來的結果"
)

geo_tool = Tool(
    name="Geography master",
    func=llm_chain.run,
    description="你是地理問題的好用工具，要告訴我國家的首都"
)

tools = [math_tool, geo_tool]

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


zero_shot_agent = create_react_agent(
    llm=model,
    tools=tools,
    prompt=prompt,
)

agent_executor = AgentExecutor(agent=zero_shot_agent, tools=tools, verbose=True)

response = agent_executor.invoke({"input": "請問一個三邊為 3 4 5 的三角形的面積是多少？"})

print(response)


response = agent_executor.invoke({"input": "請告訴我新加坡的首都是哪裡？"})

print(response)


