import type {
  CreateChatSessionRequest,
  CreateChatSessionResponse,
  SendChatMessageRequest,
  SendChatMessageResponse,
  SubmitChatReadingRequest,
  SubmitChatReadingResponse,
} from "@/types";
import axiosClient from "./config.api";
import { API_URL } from "@/constants";

export const aiChatApi = {
  /** Creates a new chat session with the user's question and returns the AI's initial response. */
  createChatSession: (
    payload: CreateChatSessionRequest,
  ): Promise<CreateChatSessionResponse> => {
    return axiosClient.post(API_URL.AI_CHAT.SESSION, payload);
  },

  /** Sends a message in an ongoing chat session and returns the AI's response. */
  sendChatMessage: (
    payload: SendChatMessageRequest,
  ): Promise<SendChatMessageResponse> => {
    return axiosClient.post(API_URL.AI_CHAT.CHAT, payload);
  },

  /** Submits drawn cards for reading and returns the AI's interpretation. */
  submitChatReading: (
    payload: SubmitChatReadingRequest,
  ): Promise<SubmitChatReadingResponse> => {
    return axiosClient.post(API_URL.AI_CHAT.READING, payload);
  },
};
