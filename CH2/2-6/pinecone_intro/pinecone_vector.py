from pinecone import Pinecone, PodSpec
from openai import AzureOpenAI

def get_embedding(text_clause):
    client = AzureOpenAI(
        api_key="yourkey",
        api_version="2023-05-15",
        azure_endpoint="https://langchainbook.openai.azure.com/"  # your endpoint
    )

    response = client.embeddings.create(
        input=text_clause,
        model="ada-002"  # 這邊放 deployment name
    )
    return response.model_dump()["data"][0]["embedding"]


def create_index():
    pc = Pinecone(
        api_key="yourkey",
    )

    index_name = "langchainbook"
    if index_name not in pc.list_indexes().names():

        pc.create_index(name=index_name,
                        dimension=1536,
                        metric="cosine",
                        spec=PodSpec(
                            environment="gcp-starter",
                            )
                        )  


def init_pinecone(index_name):
    pc = Pinecone(
        api_key="yourkey",
    )
    index = pc.Index(index_name)
    return index

def add_to_pinecone(index, embeddings, text_array):
    ids = [str(i) for i in range(len(embeddings))]
    embeddings = [embedding for embedding in embeddings]

    text_array_to_metadata = [{"content": text} for text in text_array]

    # 插入的資料會像這樣子，其中 content 是我們要搜尋的文字，這是一種 metadata。
    # ("2", [0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5, 0.5], {"content": "歌詞"})
    ids_embeddings_metadata_tuple = zip(
        ids, embeddings, text_array_to_metadata)

    index.upsert(ids_embeddings_metadata_tuple)


def search_from_pinecone(index, query_embedding, k=1):
    results = index.query(vector=query_embedding,
                          top_k=k, include_metadata=True)
    return results


def main():

    text_array = ["我會披星戴月的想你，我會奮不顧身的前進，遠方煙火越來越唏噓，凝視前方身後的距離",
                  "而我，在這座城市遺失了你，順便遺失了自己，以為荒唐到底會有捷徑。而我，在這座城市失去了你，輸給慾望高漲的自己，不是你，過分的感情"]

    embedding_array = [get_embedding(text) for text in text_array]
    
    index = init_pinecone("langchainbook")

    #第一次建立 index 時，要儲存資料到 Pinecone
    add_to_pinecone(index, embedding_array, text_array)

    query_text = "我失去了你"
    query_embedding = get_embedding(query_text)
    result = search_from_pinecone(index, query_embedding, k=1)

    print(f"尋找 {query_text}:", result)

if __name__ == '__main__':
    main()



