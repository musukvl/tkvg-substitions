CREATE TABLE IF NOT EXISTS subscription_receive_log (
    id BIGSERIAL PRIMARY KEY,
    received_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    class_name VARCHAR(50) NOT NULL,
    chat_id BIGINT NOT NULL,
    message_text TEXT NOT NULL
);

CREATE INDEX IF NOT EXISTS idx_subscription_receive_log_received_at ON subscription_receive_log (received_at DESC);
CREATE INDEX IF NOT EXISTS idx_subscription_receive_log_chat_id ON subscription_receive_log (chat_id);
CREATE INDEX IF NOT EXISTS idx_subscription_receive_log_class_name ON subscription_receive_log (class_name);
