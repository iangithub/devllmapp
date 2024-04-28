from openai import AzureOpenAI

client = AzureOpenAI(
    api_key="yourkey",
    api_version="2023-05-15",
    azure_endpoint="https://langchainbook.openai.azure.com/" # your endpoint
)

response = client.embeddings.create(
    input="鯊魚寶寶 doo doo doo doo doo doo, 鯊魚寶寶",
    model="ada-002"  # 這邊放 deployment name
)

dimensions = len(response.model_dump()["data"][0]["embedding"])
print("Dimensions: ", dimensions)
# print(response.model_dump_json(indent=2))
