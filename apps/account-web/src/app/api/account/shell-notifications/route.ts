import { NextResponse } from "next/server";

import { accountApiClient } from "@/lib/account-api";
import { getAccountApiRequestOptions } from "@/lib/auth/account-api-request-options";

export async function GET(): Promise<NextResponse> {
  try {
    const requestOptions = await getAccountApiRequestOptions();
    const notifications = await accountApiClient.getNotifications(undefined, {
      ...requestOptions,
      timeoutMs: 5_000,
    });

    return NextResponse.json(
      {
        items: notifications.items.slice(0, 5).map((item) => ({
          id: item.id,
          title: item.title,
          description: item.description,
          occurredAt: item.occurredAt,
          isRead: item.isRead,
          href: "/notifications",
        })),
        unreadCount: notifications.unreadCount,
      },
      {
        headers: {
          "cache-control": "private, no-store",
        },
      },
    );
  } catch {
    return NextResponse.json(
      {
        items: [],
        unreadCount: 0,
      },
      {
        status: 503,
        headers: {
          "cache-control": "private, no-store",
        },
      },
    );
  }
}
