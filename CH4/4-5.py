#以筆者們的上一本書做為retrival資料來源的範例
#https://www.drmaster.com.tw/bookinfo.asp?BookID=MP22315


from langchain_community.document_loaders import WebBaseLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_openai import AzureChatOpenAI, AzureOpenAIEmbeddings
from langchain_community.vectorstores import Qdrant
from langchain_core.prompts import ChatPromptTemplate, MessagesPlaceholder
from langchain.chains.combine_documents import create_stuff_documents_chain
from langchain.chains import create_retrieval_chain
from langchain_core.runnables.history import RunnableWithMessageHistory
from langchain_community.chat_message_histories import SQLChatMessageHistory


loader = WebBaseLoader("https://www.drmaster.com.tw/bookinfo.asp?BookID=MP22315")

docs = loader.load()

text_splitter = RecursiveCharacterTextSplitter()
documents = text_splitter.split_documents(docs)

model = AzureChatOpenAI(
    api_key="your key",
    openai_api_version="2023-05-15",
    azure_deployment="gpt-4",
    azure_endpoint="https://langchainbook.openai.azure.com/",
    temperature=0,
)

embeddings_model = AzureOpenAIEmbeddings(
    api_key="your key",
    azure_deployment="ada-002", # 這邊放 deployment name
    openai_api_version="2023-05-15",
    azure_endpoint="https://langchainbook.openai.azure.com/",
)

qdrant = Qdrant.from_documents(
    docs,
    embeddings_model,
    url="your qdrant cloud url", 
    api_key="your key",
    collection_name="book",
    force_recreate=True,
)

retriever = qdrant.as_retriever()

prompt = ChatPromptTemplate.from_messages([
    ("system", "請回答依照 context 裡的資訊來回答問題:{context}。問題{input}"),
    MessagesPlaceholder(variable_name="history"),
    ("human", "{input}")
    ])

document_chain = create_stuff_documents_chain(model, prompt)

retrieval_chain = create_retrieval_chain(retriever, document_chain)

chain_with_history = RunnableWithMessageHistory(
    retrieval_chain,
    lambda session_id: SQLChatMessageHistory(
        session_id="session_id", connection_string="sqlite:///langchain.db"
    ),
    input_messages_key="input",
    output_messages_key="answer",
    history_messages_key="history",
)

config = {"configurable": {"session_id": "session_id"}}


response = chain_with_history.invoke({"input": "請問這本書的作者？"}, config=config)
print(response["answer"])

response = chain_with_history.invoke({"input": "我剛剛的問題是什麼"}, config=config)
print(response["answer"])
