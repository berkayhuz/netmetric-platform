"use client";

import React from "react";
import { ArrowRight } from "lucide-react";
import { Button } from "@netmetric/ui";
import { publicEnv } from "@/lib/public-env";

interface HomeHeroProps {
  content?: any;
  locale?: string | null;
}

export function HomeHero({ content, locale }: HomeHeroProps) {
  return (
    <section className="relative flex min-h-[70vh] w-full flex-col justify-between overflow-hidden pt-28 pb-10 md:pt-36 select-none text-foreground transition-colors duration-300">
      {/* Top thin border gradient */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute left-1/2 top-0 h-px w-[700px] max-w-full -translate-x-1/2 -translate-y-1/2 bg-gradient-to-r from-transparent via-zinc-200 dark:via-zinc-800/50 to-transparent"
      />

      {/* Glow behind head */}
      <div
        aria-hidden="true"
        className="pointer-events-none absolute left-0 right-0 top-0 h-[500px] bg-[radial-gradient(ellipse_at_top,rgba(0,0,0,0.03),transparent_60%)] dark:bg-[radial-gradient(ellipse_at_top,rgba(255,255,255,0.05),transparent_60%)]"
      />

      <div className="container mx-auto flex max-w-6xl flex-col px-6 pt-8 md:pt-0">
        {/* Badge / Pill */}
        <p className="-ml-1 mb-6 w-fit rounded-xl border border-zinc-200/80 dark:border-white/10 bg-zinc-100/50 dark:bg-zinc-900/50 px-2.5 py-1 text-xs text-muted-foreground font-medium">
          Free until your first customer
        </p>

        {/* Headline */}
        <h1 className="max-w-4xl text-balance text-3xl font-medium leading-snug md:text-4xl md:leading-tight xl:text-6xl xl:leading-tight text-foreground tracking-tight">
          Start growing your agency today—without a complicated setup.
        </h1>

        {/* Subtitle / Description */}
        <div className="mt-6 flex flex-col">
          <h2 className="w-full max-w-2xl text-balance text-base font-normal text-muted-foreground md:text-[18px] md:leading-[30px]">
            Accept payments, manage tasks, and communicate with your clients with your very own
            white-labelled client portal.
            <span className="ml-2 font-medium text-muted-foreground">
              5-min setup, ready to go this evening.
            </span>
          </h2>

          {/* Action Buttons */}
          <div className="mb-6 mt-10 flex flex-row items-center gap-3">
            <Button
              asChild
              className="group/button inline-flex border border-transparent items-center justify-center rounded-xl text-sm font-medium transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring gap-2 bg-zinc-900 text-zinc-50 hover:bg-zinc-800/90 dark:bg-zinc-50 dark:text-zinc-950 dark:hover:bg-zinc-200 active:scale-[0.98] h-11 px-6 shadow-sm cursor-pointer"
            >
              <a href={publicEnv.authUrl}>Set up your agency</a>
            </Button>

            <Button
              asChild
              className="group/button inline-flex border border-transparent items-center justify-center rounded-xl text-sm font-medium transition-all focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-ring  hover:text-zinc-950 border-transparent text-zinc-800 dark:text-zinc-200 h-11 px-6 gap-2 bg-zinc-100 dark:bg-zinc-900/60 dark:hover:text-zinc-50 active:scale-[0.98] cursor-pointer"
            >
              <a href="/contact" className="inline-flex items-center gap-2">
                Book a demo
                <ArrowRight className="h-3.5 w-3.5 transition-transform group-hover/button:translate-x-1" />
              </a>
            </Button>
          </div>
        </div>
      </div>

      {/* Trust Footer */}
      <div className="w-full flex flex-wrap items-center justify-center gap-x-2.5 gap-y-1.5 text-xs sm:text-sm text-muted-foreground font-normal px-6">
        <span className="opacity-90">Trusted by agencies like</span>
        <span className="font-black tracking-tight text-muted-foreground uppercase">OFFMENU</span>
        <svg
          viewBox="0 0 24 24"
          className="h-4.5 w-4.5 text-muted-foreground fill-none stroke-current"
          strokeWidth="2.5"
          strokeLinecap="round"
          strokeLinejoin="round"
        >
          <path d="M5 12a7 7 0 0 0 14 0" />
          <circle cx="12" cy="9" r="1.5" className="fill-current" />
        </svg>
        <span className="font-bold text-muted-foreground lowercase">baked design</span>
        <span className="opacity-90">and</span>
        <span className="text-muted-foreground font-medium">700+ others.</span>
      </div>
    </section>
  );
}
