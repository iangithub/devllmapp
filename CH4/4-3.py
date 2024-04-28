from langchain_openai import AzureChatOpenAI
from langchain_core.prompts import PromptTemplate
from langchain_core.output_parsers import JsonOutputParser
from langchain_core.pydantic_v1 import BaseModel, Field

model = AzureChatOpenAI(
    api_key="yourkey",
    openai_api_version="2023-05-15",
    azure_deployment="gpt-4",
    azure_endpoint="https://langchainbook.openai.azure.com/",
    temperature=0,
)

class Translation(BaseModel):
    lang: str = Field(description="language of the translation")
    text: str = Field(description="translated text")

parser = JsonOutputParser(pydantic_object=Translation)

prompt = PromptTemplate(
    template="請把後面的句子翻譯成{language}。{content}\n{format_instructions}\n",
    input_variables=["language", "content"],
    partial_variables={"format_instructions": parser.get_format_instructions()},
)


chain = prompt | model | parser
output = chain.invoke({"language":"法文", "content":"我愛你。"})

print(output)

# stream
for item in chain.stream({"language":"法文", "content":"我愛你。"}):
    print(item)


