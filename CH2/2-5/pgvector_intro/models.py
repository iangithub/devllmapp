from sqlalchemy import Column, BigInteger
from sqlalchemy.ext.declarative import DeclarativeMeta, declarative_base
from pgvector.sqlalchemy import Vector

Base: DeclarativeMeta = declarative_base()


class Embedding(Base):
    __tablename__ = 'embeddings'
    id = Column(BigInteger, primary_key=True, index=True)
    vector = Column(Vector(1536))

