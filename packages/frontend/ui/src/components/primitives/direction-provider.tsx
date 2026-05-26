"use client";

import { DirectionProvider as RadixDirectionProvider } from "@radix-ui/react-direction";
import * as React from "react";

type DirectionProviderProps = React.ComponentProps<typeof RadixDirectionProvider>;

function DirectionProvider({ children, dir = "ltr" }: DirectionProviderProps) {
  return <RadixDirectionProvider dir={dir}>{children}</RadixDirectionProvider>;
}

export { DirectionProvider };
