import i18n from "@/i18n";
import type { ApiErrorResponse, ValidationError } from "@/types";
import { toCamelCase } from "./common.utils";
import type { FormInstance } from "antd";
// import type { FieldData } from "rc-field-form/lib/interface";

function translateErrorMessage(code: string): string {
  return i18n.exists(code)
    ? i18n.t(code)
    : i18n.t("error.server.internalServerError");
}

function extractApiError(error: unknown): ApiErrorResponse {
  const err = error as ApiErrorResponse;

  return (
    err ?? {
      success: false,
      message: "error.server.internalServerError",
      data: null,
    }
  );
}

/** Use to catch the error by message
 * @params error: the error
 * @return The error message or 'InternalServerError' message already translated
 */
export function getErrorMessage(error: unknown): string {
  const err = extractApiError(error);

  return translateErrorMessage(
    err.message ?? "error.server.internalServerError",
  );
}

type FieldData<T> = Parameters<FormInstance<T>["setFields"]>[0][number];
/**
 * Use to catch the validation error from form
 * @param error: the error
 * @returns The array contains name's field and the list of errors, return empty if there is no validation error
 */
export function getFormFieldErrors<T>(error: unknown): FieldData<T>[] {
  const err = extractApiError(error);

  if (!Array.isArray(err.data)) {
    return [];
  }

  return (err.data as ValidationError[]).map((e) => ({
    name: toCamelCase(e.key) as FieldData<T>["name"],
    errors: [translateErrorMessage(e.value)],
  }));
}
