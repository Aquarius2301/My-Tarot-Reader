import { aiChatApi } from "@/api";
import type {
  CreateChatSessionRequest,
  SendChatMessageRequest,
  SubmitChatReadingRequest,
} from "@/types";
import { useMutation } from "@tanstack/react-query";

/** Creates a new AI chat session with the user's question. */
export const useCreateChatSession = () => {
  return useMutation({
    mutationFn: (payload: CreateChatSessionRequest) =>
      aiChatApi.createChatSession(payload),
  });
};

/** Sends a message in an ongoing AI chat session. */
export const useSendChatMessage = () => {
  return useMutation({
    mutationFn: (payload: SendChatMessageRequest) =>
      aiChatApi.sendChatMessage(payload),
  });
};

/** Submits drawn cards for a custom AI reading. */
export const useSubmitChatReading = () => {
  return useMutation({
    mutationFn: (payload: SubmitChatReadingRequest) =>
      aiChatApi.submitChatReading(payload),
  });
};
