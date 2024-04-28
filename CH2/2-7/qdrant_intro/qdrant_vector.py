from qdrant_client import QdrantClient
from qdrant_client.http import models
from qdrant_client.http.models import PointStruct
from openai import AzureOpenAI


def connection(collection_name):
    client = QdrantClient(url="yoururl"
                          , api_key="yourkey")

    # 建立 collection，在 qdrant 中 recreate_collection就是重新創建一個 collection
    client.recreate_collection(
        collection_name=collection_name,
        vectors_config=models.VectorParams(
            distance=models.Distance.COSINE,
            size=1536),
        optimizers_config=models.OptimizersConfigDiff(memmap_threshold=20000),
        hnsw_config=models.HnswConfigDiff(on_disk=True, m=16, ef_construct=100)
    )
    return client


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

def upsert_vector(client, vectors, collection_name, data):
    for i, vector in enumerate(vectors):
        client.upsert(
            collection_name=collection_name,
            points=[PointStruct(id=i,
                                vector=vectors[i],
                                payload=data[i])]
        )

    print("upsert finish")

def search_from_qdrant(client, vector, collection_name, k=1):
    search_result = client.search(
        collection_name=collection_name,
        query_vector=vector,
        limit=k,
        append_payload=True,
    )
    return search_result

def main():
    collection_name = "Lyrics"

    qclient = connection(collection_name)

    data_objs = [
        {
            "id": 1,
            "lyric": "我會披星戴月的想你，我會奮不顧身的前進，遠方煙火越來越唏噓，凝視前方身後的距離"
        },
        {
            "id": 2,
            "lyric": "而我，在這座城市遺失了你，順便遺失了自己，以為荒唐到底會有捷徑。而我，在這座城市失去了你，輸給慾望高漲的自己，不是你，過分的感情"
        }
    ]
    embedding_array = [get_embedding(text["lyric"])
                       for text in data_objs]


    upsert_vector(qclient, embedding_array,collection_name, data_objs)

    query_text = "我遠離了你"
    query_embedding = get_embedding(query_text)
    results = search_from_qdrant(qclient, query_embedding,collection_name,k=1)
    print(f"尋找 {query_text}:", results)



if __name__ == '__main__':
    main()
