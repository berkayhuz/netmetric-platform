export default function AuthLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <div className="flex min-h-screen flex-col">
      <div className="pointer-events-none fixed inset-0 -z-10 overflow-hidden bg-[#f6f8fc]">
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_-12%,rgba(255,255,255,1),rgba(246,248,252,0.68)_38%,rgba(246,248,252,1)_76%)]"></div>
        <div className="nm-orb nm-orb-1 absolute -top-60 left-1/2 h-[520px] w-[520px] -translate-x-1/2 rounded-full bg-sky-400/20 blur-[95px] sm:h-[760px] sm:w-[760px] sm:blur-[110px]"></div>
        <div className="nm-orb nm-orb-2 absolute left-[-300px] top-[16%] h-[460px] w-[460px] rounded-full bg-blue-500/14 blur-[105px] sm:left-[-260px] sm:h-[620px] sm:w-[620px] sm:blur-[120px]"></div>
        <div className="nm-orb nm-orb-3 absolute right-[-300px] top-[24%] h-[440px] w-[440px] rounded-full bg-cyan-300/18 blur-[105px] sm:right-[-240px] sm:h-[580px] sm:w-[580px] sm:blur-[120px]"></div>
        <div className="nm-orb nm-orb-4 absolute bottom-[-300px] left-[20%] h-[520px] w-[520px] rounded-full bg-indigo-400/14 blur-[115px] sm:left-[32%] sm:h-[700px] sm:w-[700px] sm:blur-[130px]"></div>
        <div className="nm-orb nm-orb-5 absolute bottom-[8%] right-[8%] h-[280px] w-[280px] rounded-full bg-violet-300/12 blur-[85px] sm:right-[18%] sm:h-[360px] sm:w-[360px] sm:blur-[95px]"></div>
        <div className="absolute inset-0 bg-white/42 backdrop-blur-[2px]"></div>
        <div className="absolute inset-0 opacity-[0.4] [background-image:linear-gradient(to_right,rgba(24,24,27,0.055)_1px,transparent_1px),linear-gradient(to_bottom,rgba(24,24,27,0.055)_1px,transparent_1px)] [background-size:72px_72px]"></div>
        <div className="absolute inset-0 opacity-[0.12] [background-image:linear-gradient(to_right,rgba(59,130,246,0.08)_1px,transparent_1px),linear-gradient(to_bottom,rgba(59,130,246,0.08)_1px,transparent_1px)] [background-size:20px_20px]"></div>
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_50%_35%,transparent_0%,transparent_34%,rgba(246,248,252,0.72)_78%,rgba(246,248,252,0.96)_100%)]"></div>
        <div className="absolute left-[-25%] top-[-40%] h-[85%] w-[150%] rotate-[-10deg] bg-[linear-gradient(90deg,transparent,rgba(255,255,255,0.58),transparent)] opacity-65 blur-2xl"></div>
        <div className="absolute inset-x-0 top-0 h-px bg-gradient-to-r from-transparent via-white to-transparent"></div>
        <div className="absolute inset-0 bg-[radial-gradient(circle_at_center,transparent_42%,rgba(226,232,240,0.48)_100%)]"></div>
        <div className="absolute inset-0 opacity-[0.04]"></div>
      </div>
      <div className="flex-1">{children}</div>
    </div>
  );
}
