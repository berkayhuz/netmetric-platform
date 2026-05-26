import Image from "next/image";

import { AuthFooter } from "@/features/navigation/components/auth-footer";

type AuthPageShellProps = {
  children: React.ReactNode;
  title?: string;
  description?: string;
};

export function AuthPageShell({ children }: AuthPageShellProps) {
  return (
    <div className="overflow-x-hidden">
      <div className="min-h-screen">
        <section className="grid grid-cols-1 lg:grid-cols-2 min-h-screen">
          <div className="relative min-h-screen">
            <div className="lg:hidden absolute inset-0 ">
              <Image
                src="/images/auth.png"
                alt="Creative workspace"
                fill
                sizes="(max-width: 1023px) 100vw, 0vw"
                priority
                className="absolute inset-0 h-full w-full object-cover"
              />
            </div>
            <div className="absolute top-6 lg:top-8 z-9999 ml-4">
              <a href="https://www.netmetric.net/">
                <Image
                  src="/brand/netmetric_full_logo_black.png"
                  className="h-auto w-[148px] text-white"
                  alt="NetMetric"
                  width={512}
                  height={102}
                  priority
                />
              </a>
            </div>
            <div className="flex items-center justify-center min-h-screen p-6 lg:p-8 relative z-10">
              <div className="w-full max-lg:max-w-md bg-white rounded-2xl py-6 lg:p-8 lg:bg-transparent lg:rounded-none space-y-6 shadow-lg lg:shadow-none ">
                {children}
              </div>
            </div>
            <AuthFooter />
          </div>
          <div className="hidden lg:block relative min-h-screen p-4 ">
            <div className="h-full w-full rounded-2xl p-4">
              <div className="relative h-full w-full">
                <Image
                  src="/images/auth.png"
                  alt="Creative workspace"
                  fill
                  sizes="(min-width: 1024px) 50vw, 0vw"
                  priority
                  className="absolute inset-0 h-full w-full object-cover rounded-2xl"
                />
                <div className="absolute inset-0 flex items-center justify-center py-6 px-12 xl:px-16 2xl:px-20">
                  <div className="w-full max-w-3xl">
                    <div className="bg-gradient-to-br from-white/80 to-white/30 backdrop-blur-md rounded-[12px] xl:rounded-[16px] 2xl:rounded-[20px] p-3 border border-white/30">
                      <div className="flex items-center gap-4">
                        <div className="flex-1 flex items-center gap-3 select-none">
                          <span className="text-slate-400 text-[22px] xl:text-[30px] 2xl:text-[40px] pl-2 font-normal bg-gradient-to-r from-[#BCD2EB] via-[#A7BDD5] to-[#A7BDD5] bg-clip-text text-transparent">
                            Turn your data into growth
                          </span>
                          <div className="w-0.5 h-[30px] xl:h-[40px] 2xl:h-[54px] bg-gradient-to-b from-slate-300 to-slate-400 animate-[caret-blink_1s_steps(1,end)_infinite]" />
                        </div>
                        <div className="flex-shrink-0">
                          <div className="w-[30px] h-[30px] xl:w-[40px] xl:h-[40px] 2xl:w-[54px] 2xl:h-[54px] rounded-sm xl:rounded-[8px] 2xl:rounded-[12px] bg-gradient-to-br from-slate-400/80 to-slate-300/80 flex items-center justify-center">
                            <svg
                              width="18"
                              height="18"
                              viewBox="0 0 24 24"
                              fill="none"
                              className="text-white"
                            >
                              <path
                                d="M12 4L12 20M12 4L18 10M12 4L6 10"
                                stroke="currentColor"
                                strokeWidth="2"
                                strokeLinecap="round"
                                strokeLinejoin="round"
                              ></path>
                            </svg>
                          </div>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </section>
      </div>
    </div>
  );
}
