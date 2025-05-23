from sqlalchemy import Column, Float, Integer, String, TIMESTAMP
from db.db import Base

class UserEvent(Base):
    __tablename__ = "user_ratings"

    id = Column(Integer, primary_key=True, index=True)
    user_id = Column(String, index=True)
    book_id = Column(String, index=True)
    rating = Column(Integer, nullable=False)
    #timestamp = Column(TIMESTAMP)
