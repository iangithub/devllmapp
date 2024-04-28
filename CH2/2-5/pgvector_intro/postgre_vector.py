from openai import AzureOpenAI
from sqlalchemy import create_engine, select
from sqlalchemy.orm import sessionmaker
from sqlalchemy.sql import text

from models import Embedding

# Initialize DB connection
engine = create_engine("postgresql://user:password@localhost:5432/vector")
Session = sessionmaker(bind=engine)
session = Session()


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


def add_to_pg_vector(session, embeddings):
    embeddings_obj = [Embedding(vector=vector) for vector in embeddings]
    session.add_all(embeddings_obj)
    session.commit()


def search_from_pg_vector(session, text_array, query_embedding, k=1):
    results = session.scalars(select(Embedding).order_by(
        Embedding.vector.cosine_distance(query_embedding)).limit(k))
    distance = session.scalars(
        select((Embedding.vector.cosine_distance(query_embedding))))

    # 合併 distance 和 text_array，並且把 cosine_distance 轉成 similarity。
    # results.id - 1 是因為 id 從 1 開始，而我們的 text_array 從 0 開始
    return [(text_array[result.id - 1], 1 - float(dist)) for dist, result in zip(distance, results)]


def main():
    text_array = ["我會披星戴月的想你，我會奮不顧身的前進，遠方煙火越來越唏噓，凝視前方身後的距離",
                  "而我，在這座城市遺失了你，順便遺失了自己，以為荒唐到底會有捷徑。而我，在這座城市失去了你，輸給慾望高漲的自己，不是你，過分的感情"]

    embedding_array = [get_embedding(text_clause) for text_clause in text_array]

    add_to_pg_vector(session, embedding_array)

    query_text = "我遠離了你"
    query_embedding = get_embedding(query_text)  # 問句也要轉成 embedding
    search_results = search_from_pg_vector(session, text_array, query_embedding)
    print(f"尋找 {query_text}:", search_results)


if __name__ == '__main__':
    try:
        # 執行前先把資料庫清空
        session.query(Embedding).delete()
        # 把 postgre 的 id 重設
        session.execute(text("ALTER SEQUENCE embeddings_id_seq RESTART WITH 1"))
        main()
    finally:
        session.close()
