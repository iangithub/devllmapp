#以筆者們的上一本書做為retrival資料來源的範例
#https://www.drmaster.com.tw/bookinfo.asp?BookID=MP22315


from langchain_community.document_loaders import WebBaseLoader
from langchain.text_splitter import RecursiveCharacterTextSplitter
from langchain_openai import AzureChatOpenAI, AzureOpenAIEmbeddings
from langchain_community.vectorstores import Qdrant
from langchain_core.prompts import ChatPromptTemplate
from langchain.chains.combine_documents import create_stuff_documents_chain
from langchain.chains import create_retrieval_chain



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

prompt = ChatPromptTemplate.from_template("""請回答依照 context 裡的資訊來回答問題:
<context>
{context}
</context>
Question: {input}""")

document_chain = create_stuff_documents_chain(model, prompt)

retrieval_chain = create_retrieval_chain(retriever, document_chain)

response = retrieval_chain.invoke({"input": "請問這本書的作者？"})
print(response["answer"])