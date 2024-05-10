from langchain_openai import AzureChatOpenAI

from langchain.prompts import ChatPromptTemplate
from langchain.chains import LLMChain




def run_llm_chain():

    prompt = ChatPromptTemplate.from_template(
        "請幫我寫一首簡單的{song}歌。"
    )
    llm_chain = get_llm_chain(prompt)

    result = llm_chain.run("搖滾")
    print(result)

if __name__ == "__main__":
    run_llm_chain()
