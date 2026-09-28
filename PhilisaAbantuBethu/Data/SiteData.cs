using PhilisaAbantuBethu.Models;

namespace PhilisaAbantuBethu.Data;

public static class SiteData
{

    public static readonly List<Programme> Programmes = new()
    {

        new Programme
        {

            Id = "womens-empowerment",
            Title = "Women's Empowerment",
            Tagline = "Building strength from within",
            Description = "Supporting women through skills development, legal assistance, and economic independence — because empowered women build stronger communities.",
            Overview = "Our Women's Empowerment programme is at the heart of everything we do. We believe that when women are empowered, entire communities flourish. Through a comprehensive range of services, we help women reclaim their dignity, build financial independence, and become leaders in their communities.",
            
            Objectives = new()
            {

                "Provide skills training and vocational education",
                "Offer legal assistance and rights awareness workshops",
                "Support survivors of gender-based violence",
                "Build leadership and entrepreneurship capacity"

            },

            Activities = new()
            {

                "Weekly sewing, craft, and tailoring workshops",
                "Financial literacy and savings group sessions",
                "Legal clinics with pro-bono attorneys",
                "Support group meetings and trauma counselling",
                "Annual Womens Day celebration and awards"

            },

            ImagePath = "/img/programmes/womens-empowerment/hero.jpg",
            HeroFocalX = 42,
            HeroFocalY = 22,
            GalleryImages = new()
            {
                "/img/programmes/womens-empowerment/gallery-1.jpg",
                "/img/programmes/womens-empowerment/gallery-2.jpg",
                "/img/programmes/womens-empowerment/gallery-3.jpg",
                "/img/programmes/womens-empowerment/gallery-4.jpg"
            },
            Accent = "#7C3AED"

        },

        new Programme
        {

            Id = "youth-programme",
            Title = "Youth Programme",
            Tagline = "Igniting potential in young people",
            Description = "Equipping young people aged 15–35 with skills, mentorship, and opportunities to break the cycle of poverty and build purposeful futures.",
            Overview = "Our Youth Programme creates safe spaces for young people to develop their potential. Through mentorship, skills training, and life coaching, we help youth navigate challenges and build futures they can be proud of.",
            
            Objectives = new()
            {

                "Reduce youth unemployment through accredited skills development",
                "Provide mentorship and life coaching",
                "Address substance abuse and risky behaviour",
                "Build civic responsibility and community pride"

            },

            Activities = new()
            {

                "Weekly mentorship circles and group sessions",
                "CV writing, interview preparation, and job placement",
                "Sports and recreation events",
                "Arts, culture, and creative workshops",
                "Annual youth leadership camp"

            },

            ImagePath = "/img/programmes/youth-programme/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 30,
            GalleryImages = new()
            {
                "/img/programmes/youth-programme/gallery-1.jpg",
                "/img/programmes/youth-programme/gallery-2.jpg",
                "/img/programmes/youth-programme/gallery-3.jpg",
                "/img/programmes/youth-programme/gallery-4.jpg"
            },
            Accent = "#1D4ED8"

        },

        new Programme
        {

            Id = "after-school",
            Title = "After-school Programmes",
            Tagline = "Learning beyond the classroom",
            Description = "Safe, structured learning environments for children after school hours — with tutoring, enrichment activities, and a warm afternoon meal.",
            Overview = "Our After-school Programme provides a structured, safe, and stimulating environment for children from grades 1–12. Trained tutors and volunteers help children with homework, reading, and mathematics while also offering enrichment activities that nurture creativity.",
            
            Objectives = new()
            {

                "Improve academic performance in core subjects",
                "Provide safe, supervised space after school hours",
                "Nurture creativity through arts and culture",
                "Build healthy habits and essential life skills"

            },

            Activities = new()
            {

                "Daily homework assistance and small-group tutoring",
                "Reading clubs and community library visits",
                "Science experiments and STEM activities",
                "Art, drama, and music sessions",
                "Nutritious afternoon snack programme"
            
            },

            ImagePath = "/img/programmes/after-school/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 42,
            GalleryImages = new()
            {
                "/img/programmes/after-school/gallery-1.jpg",
                "/img/programmes/after-school/gallery-2.jpg",
                "/img/programmes/after-school/gallery-3.jpg",
                "/img/programmes/after-school/gallery-4.jpg"
            },
            Accent = "#059669"

        },

        new Programme
        {

            Id = "senior-programme",
            Title = "Senior Programme",
            Tagline = "Honouring our elders",
            Description = "Caring for older community members through social connection, wellness activities, and practical support — because every elder deserves dignity.",
            Overview = "Our Senior Programme honours the wisdom and dignity of our older community members. We provide companionship, wellness support, and practical assistance to ensure that our elders age with dignity, connection, and care.",
            
            Objectives = new()
            {

                "Combat isolation and loneliness among seniors",
                "Support access to healthcare and social grants",
                "Provide practical assistance with daily needs",
                "Celebrate and preserve cultural heritage"

            },

            Activities = new()
            {

                "Weekly senior social gatherings and tea afternoons",
                "Health screenings and wellness clinics",
                "SASSA grant application assistance",
                "Intergenerational storytelling events",
                "Home visits for mobility-impaired elders"

            },

            ImagePath = "/img/programmes/senior-programme/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 42,
            GalleryImages = new()
            {
                "/img/programmes/senior-programme/gallery-1.jpg",
                "/img/programmes/senior-programme/gallery-2.jpg",
                "/img/programmes/senior-programme/gallery-3.jpg",
                "/img/programmes/senior-programme/gallery-4.jpg"
            },
            Accent = "#B45309"

        },

        new Programme
        {

            Id = "community-feeding",
            Title = "Community Feeding",
            Tagline = "No one goes hungry here",
            Description = "Providing hot, nutritious meals and monthly food parcels to vulnerable families and individuals facing food insecurity in our community.",
            Overview = "The Community Feeding programme is our response to hunger. We operate daily feeding stations and distribute food parcels to families in need, ensuring that no member of our community goes to bed hungry.",
            
            Objectives = new()
            {

                "Eliminate hunger among vulnerable community members",
                "Provide nutritious, balanced meals every day",
                "Distribute food parcels to homebound individuals",
                "Educate on nutrition and sustainable food practices"

            },

            Activities = new()
            {

                "Daily community kitchen serving 200+ hot meals",
                "Monthly food parcel distribution drives",
                "Community vegetable garden and food production",
                "Nutrition education and cooking workshops",
                "Holiday and festive season feeding drives"

            },

            ImagePath = "/img/programmes/community-feeding/hero.jpg",
            HeroFocalX = 68,
            HeroFocalY = 32,
            GalleryImages = new()
            {
                "/img/programmes/community-feeding/gallery-1.jpg",
                "/img/programmes/community-feeding/gallery-2.jpg",
                "/img/programmes/community-feeding/gallery-3.jpg",
                "/img/programmes/community-feeding/gallery-4.jpg"
            },
            Accent = "#EA580C"

        },

        new Programme
        {

            Id = "baby-saver",
            Title = "Baby Saver",
            Tagline = "Every life is precious",
            Description = "Supporting young and vulnerable mothers with care, essential resources, and family support — ensuring every baby has the best possible start.",
            Overview = "The Baby Saver programme provides a lifeline for young and vulnerable mothers. We offer a safe place for mothers in crisis and ensure that every baby has access to the essentials needed to thrive.",
            
            Objectives = new()
            {

                "Prevent infant abandonment and abuse",
                "Support young and first-time mothers",
                "Provide essential baby items and nutritional support",
                "Connect families with social services"

            },

            Activities = new()
            {

                "Parenting classes and peer support groups",
                "Baby essentials distribution (nappies, formula, clothing)",
                "Breastfeeding support and lactation consulting",
                "Crisis intervention for mothers in distress",
                "Adoption and foster care facilitation"

            },

            ImagePath = "/img/programmes/baby-saver/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 45,
            GalleryImages = new()
            {
                "/img/programmes/baby-saver/gallery-1.jpg",
                "/img/programmes/baby-saver/gallery-2.jpg"
            },
            Accent = "#DB2777"

        },

        new Programme
        {

            Id = "safe-houses",
            Title = "Emergency Safe Houses",
            Tagline = "Safety, always",
            Description = "Providing immediate, confidential shelter and support for women and children fleeing domestic violence and life-threatening situations.",
            Overview = "Our Emergency Safe Houses provide immediate refuge for women and children fleeing domestic violence, abuse, and dangerous situations. We offer a secure environment, counselling, and pathways to long-term safety and independence.",
            
            Objectives = new()
            {

                "Provide immediate safe shelter for survivors",
                "Offer trauma counselling and psychosocial support",
                "Assist with legal protection orders",
                "Build pathways to independent, safe living"

            },
            
            Activities = new()
            {

                "24/7 crisis line and emergency response",
                "Confidential safe house accommodation",
                "Individual and group trauma counselling",
                "Legal advocacy and court support",
                "Reintegration and life-skills rebuilding"

            },
            
            ImagePath = "/img/programmes/safe-houses/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 55,
            GalleryImages = new()
            {
                "/img/programmes/safe-houses/gallery-1.jpg",
                "/img/programmes/safe-houses/gallery-2.jpg",
                "/img/programmes/safe-houses/gallery-3.jpg",
                "/img/programmes/safe-houses/gallery-4.jpg"
            },
            Accent = "#DC2626"

        },

        new Programme
        {

            Id = "search-rescue",
            Title = "Search & Rescue",
            Tagline = "We find you. We bring you home.",
            Description = "A trained community volunteer unit responding to missing persons cases and community emergencies — working with SAPS and social services.",
            Overview = "Our Search & Rescue team is a trained volunteer unit that responds to missing persons cases and community emergencies. We work alongside law enforcement and social services to ensure that vulnerable individuals are found and supported.",
            
            Objectives = new()
            {

                "Locate missing persons in the community",
                "Support families of missing persons emotionally",
                "Coordinate with SAPS and social services",
                "Train community volunteers in search techniques"

            },
            
            Activities = new()
            {

                "24/7 emergency response deployment",
                "Volunteer training and field simulation exercises",
                "Community awareness and prevention campaigns",
                "Support groups for families of missing persons",
                "Missing persons registration and digital database"

            },
            
            ImagePath = "/img/programmes/search-rescue/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 32,
            GalleryImages = new()
            {
                "/img/programmes/search-rescue/gallery-1.jpg"
            },
            Accent = "#475569"

        },

        new Programme
        {

            Id = "social-work",
            Title = "Social Work Services",
            Tagline = "Professional care, human heart",
            Description = "Qualified social workers connecting families with resources, grants, counselling, and the essential services they need to thrive.",
            Overview = "Our qualified social workers provide professional, compassionate support to individuals and families navigating complex challenges. From grant applications to family counselling, our team connects people with the resources they need.",
            
            Objectives = new()
            {

                "Provide professional social work assessments",
                "Connect families with government grants and services",
                "Offer family therapy and conflict mediation",
                "Support child protection and welfare"

            },

            Activities = new()
            {

                "One-on-one social work consultations",
                "SASSA grant application and appeal assistance",
                "Family court support and representation",
                "Child welfare investigations and support plans",
                "Home visit assessments and case management"

            },

            ImagePath = "/img/programmes/social-work/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 45,
            GalleryImages = new()
            {
                "/img/programmes/social-work/gallery-1.jpg",
                "/img/programmes/social-work/gallery-2.jpg",
                "/img/programmes/social-work/gallery-3.jpg"
            },
            Accent = "#0D9488"

        },

        new Programme
        {

            Id = "mens-cafe",
            Title = "Men's Café",
            Tagline = "Engaging men to end gender-based violence",
            Description = "A partnership with the NPO Inside Out offering regular workshops, life-skills training, and a safe space for men to reflect on manhood and help prevent gender-based violence.",
            Overview = "Philisa Abafazi Bethu believes that preventing gender-based violence is not possible without working closely with men. Men's Café was launched in 2022 in partnership with the NPO Inside Out, who facilitate regular Manhood training workshops from the PAB centre. These sessions go beyond conversation — reflecting on patriarchal structures in our society, building life skills, and creating a safe space where men can share their feelings and perspectives. The programme also operates an Emergency Safe House for men in distress, and is proudly funded by the World Childhood Foundation.",

            Objectives = new()
            {

                "Engage men as partners in preventing gender-based violence",
                "Challenge harmful patriarchal norms through guided reflection",
                "Build life skills and emotional literacy among men",
                "Provide emergency shelter for men in crisis"

            },

            Activities = new()
            {

                "Regular Manhood training workshops facilitated by Inside Out",
                "Group discussions on masculinity and patriarchal structures",
                "Life skills training sessions",
                "Safe space circles for men to share feelings and perspectives",
                "Emergency Safe House accommodation for men in distress"

            },

            ImagePath = "/img/programmes/mens-cafe/hero.jpg",
            HeroFocalX = 50,
            HeroFocalY = 32,
            GalleryImages = new()
            {
                "/img/programmes/mens-cafe/gallery-1.jpg",
                "/img/programmes/mens-cafe/gallery-2.jpg",
                "/img/programmes/mens-cafe/gallery-3.jpg"
            },
            Accent = "#0E7490"

        }

    };

    public static Programme FindProgramme(string? id) =>
        Programmes.FirstOrDefault(p => p.Id == id) ?? Programmes[0];

}